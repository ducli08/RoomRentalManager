import { Component, EventEmitter, Inject, Optional, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { API_BASE_URL, CreateOrEditRoomRentalDto, FileParameter, SelectListItem, ServiceProxy } from '../../../shared/services';
import { ReactiveFormsModule } from '@angular/forms';
import { NzFormItemComponent, NzFormLabelComponent, NzFormControlComponent } from 'ng-zorro-antd/form';
import { NZ_MODAL_DATA, NzModalModule, NzModalRef } from 'ng-zorro-antd/modal';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { CommonModule } from '@angular/common';
import { CategoryCacheService } from '../../../shared/category-cache.service';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { forkJoin, of } from 'rxjs';
import { SelectListItemService } from '../../../shared/get-select-list-item.service';
import { NzUploadFile, NzUploadModule } from 'ng-zorro-antd/upload';
import { resolveAssetUrl } from '../../../shared/resolve-asset-url';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { RoomRentalUploadService } from '../../../shared/roomrental-upload.service';
const getBase64 = (file: File): Promise<string | ArrayBuffer | null> => new Promise((resolve, reject) => {
  const reader = new FileReader();
  reader.readAsDataURL(file);
  reader.onload = () => resolve(reader.result);
  reader.onerror = error => reject(error);
});
@Component({
  selector: 'app-editroomrentals',
  imports: [ReactiveFormsModule, NzFormItemComponent, NzFormLabelComponent,
    NzFormControlComponent, NzModalModule, NzInputModule, NzButtonModule,
    NzDatePickerModule, NzSelectModule, CommonModule, NzIconModule, NzUploadModule],
  templateUrl: './editroomrentals.component.html',
  styleUrl: './editroomrentals.component.css'
})
export class EditRoomRentalsComponent {
  @Output() saved = new EventEmitter<void>();
  lstUser: SelectListItem[] = [];
  lstRoomTypes: SelectListItem[] = [];
  lstRoomStatuses: SelectListItem[] = [];
  fileList: NzUploadFile[] = [];
  previewImage: string | undefined = '';
  previewVisible = false;
  editRoomRentalForm: FormGroup;
  baseUrl?: string;
  saving = false;
  constructor(
    private fb: FormBuilder,
    private serviceProxy: ServiceProxy,
    private roomRentalUpload: RoomRentalUploadService,
    private memoryCache: CategoryCacheService,
    private _getSelectListItem: SelectListItemService, @Inject(NZ_MODAL_DATA) public data: { roomrentalData: any },
    private modalRef: NzModalRef,
    private notification: NzNotificationService,
    @Optional() @Inject(API_BASE_URL) baseUrl: string
  ) {
    this.baseUrl = baseUrl;
    // Initialize form with basic structure first
    this.editRoomRentalForm = this.fb.group({
      id: [''],
      roomNumber: ['', Validators.required],
      roomType: ['', Validators.required],
      price: ['', Validators.required],
      statusRoom: ['', Validators.required],
      note: [''],
      area: ['', Validators.required],
      createdDate: [''],
      updatedDate: [''],
      creatorUser: [''],
      lastUpdateUser: [''],
      imagesDescription: ['']
    });
  }
  controlRequestArray: Array<{
    label: string;
    key: string;
    type: string;
    options?: () => SelectListItem[];
    placeholder?: string;
    validators?: any[];
  }> = [];

  handlePreview = async (file: NzUploadFile): Promise<void> => {
    if (!file.url && !file['preview']) {
      file['preview'] = await getBase64(file.originFileObj!);
    }
    this.previewImage = file.url || file['preview'];
    this.previewVisible = true;
  };

  handleRemove = (file: NzUploadFile): boolean => {
    // Controlled mode: MUST update fileList ourselves when user removes.
    if (file.originFileObj && file.thumbUrl && file.thumbUrl.startsWith('blob:')) {
      try {
        URL.revokeObjectURL(file.thumbUrl);
      } catch {
        // ignore
      }
    }
    this.fileList = this.fileList.filter(f => f.uid !== file.uid);
    this.editRoomRentalForm.get('imagesDescription')?.setValue(this.fileList);
    return true;
  };

  beforeUpload = (file: NzUploadFile): boolean => {
    let rawFile: File | undefined;
    let uploadFile: NzUploadFile | undefined;
    if (file instanceof File) {
      rawFile = file;
      uploadFile = {
        ...file,
        originFileObj: file,
        status: 'done',
        thumbUrl: URL.createObjectURL(file)
      }
    }
    else {
      rawFile = file.originFileObj;
      uploadFile = {
        ...file,
        originFileObj: file.originFileObj,
        status: 'done',
        thumbUrl: file.url
      };
    }
    this.fileList = [...this.fileList, uploadFile!];
    this.editRoomRentalForm.get('imagesDescription')?.setValue(this.fileList);
    return false;
  };
  ngOnInit(): void {
    const cachedRoomTypes = this.memoryCache.get<SelectListItem[]>('roomType');
    const cachedRoomStatus = this.memoryCache.get<SelectListItem[]>('roomStatus');
    const cachedUsers = this.memoryCache.get<SelectListItem[]>('user');
    const userObservable$ = cachedUsers ? of(cachedUsers) : this._getSelectListItem.getSelectListItems("user", "");
    const roomTypeObservable$ = cachedRoomTypes ? of(cachedRoomTypes) : this._getSelectListItem.getEnumSelectListItems("roomType");
    const roomStatusObservable$ = cachedRoomStatus ? of(cachedRoomStatus) : this._getSelectListItem.getEnumSelectListItems("roomStatus");

    forkJoin([userObservable$, roomTypeObservable$, roomStatusObservable$])
      .subscribe(([users, roomTypes, roomStatus]) => {
        this.lstUser = users ? users : [];
        this.lstRoomTypes = roomTypes ? roomTypes : [];
        this.lstRoomStatuses = roomStatus ? roomStatus : [];
        if (!cachedUsers) this.memoryCache.set('user', users);
        if (!cachedRoomTypes) this.memoryCache.set('roomType', roomTypes);
        if (!roomStatus) this.memoryCache.set('roomStatus', roomStatus);

        // Khởi tạo controlRequestArray sau khi có dữ liệu
        this.initializeFormControls();

        // Fill dữ liệu vào form SAU KHI đã có tất cả select lists
        this.populateFormData();
      },
        error => {
          console.error('Error fetching data:', error);
        }
      );
  }

  initializeFormControls(): void {
    // Định nghĩa các field cho form chỉnh sửa (chỉ những field có thể edit)
    this.controlRequestArray = [
      {
        label: 'Số phòng',
        key: 'roomNumber',
        type: 'text',
        placeholder: 'Nhập số phòng'
      },
      {
        label: 'Loại phòng',
        key: 'roomType',
        type: 'select',
        options: () => this.lstRoomTypes,
        placeholder: 'Chọn loại phòng'
      },
      {
        label: 'Trạng thái phòng',
        key: 'statusRoom',
        type: 'select',
        options: () => this.lstRoomStatuses,
        placeholder: 'Chọn trạng thái phòng'
      },
      {
        label: 'Ghi chú',
        key: 'note',
        type: 'text',
        placeholder: 'Nhập ghi chú',
        validators: []
      },
      {
        label: 'Diện tích',
        key: 'area',
        type: 'number',
        placeholder: 'Nhập diện tích (m²)'
      },
      {
        label: 'Giá',
        key: 'price',
        type: 'number',
        placeholder: 'Nhập giá phòng',
      },
      {
        label: "Ảnh mô tả",
        key: 'imagesDescription',
        type: 'file',
        placeholder: 'Chọn ảnh mô tả',
      }
    ];
  }

  populateFormData(): void {
    // Fill dữ liệu vào form nếu có
    if (this.data && this.data.roomrentalData) {
      console.log('Populating form with data:', this.data.roomrentalData);

      // Ensure the form values match the select options
      const formData = { ...this.data.roomrentalData };

      // Convert enum values to match select options if needed
      if (formData.roomNumber !== undefined && formData.roomNumber !== null) {
        formData.roomNumber = String(formData.roomNumber);
      }
      if (formData.roomType !== undefined) {
        formData.roomType = formData.roomType.toString();
      }
      if (formData.statusRoom !== undefined) {
        formData.statusRoom = formData.statusRoom.toString();
      }

      this.editRoomRentalForm.patchValue(formData);

      console.log('Form after patch:', this.editRoomRentalForm.value);

      // Handle image descriptions if available
      if (this.data.roomrentalData.imagesDescription && this.data.roomrentalData.imagesDescription.length > 0) {
        this.fileList = this.data.roomrentalData.imagesDescription.map((img: any, index: number) => {
          const path = typeof img === 'string' ? img : (img?.imageUrl ?? img?.url ?? '');
          const src = resolveAssetUrl(this.baseUrl, path);
          return {
            uid: `${index}`,
            name: `image-${index}`,
            status: 'done' as const,
            url: src,
            thumbUrl: src,
            // keep server relative path so submit can send correct values
            serverPath: path
          };
        });
      }
    }
  }

  onSubmit(): void {
    if (!this.editRoomRentalForm.valid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.editRoomRentalForm.controls).forEach(key => {
        this.editRoomRentalForm.get(key)?.markAsTouched();
      });
      return;
    }

    if (this.saving) return;
    this.saving = true;

    const v = this.editRoomRentalForm.value;

    const existingServerPaths = (this.fileList || [])
      .filter(f => !(f.originFileObj instanceof File))
      .map(f => (f as any).serverPath as string | undefined)
      .filter((p): p is string => !!p && String(p).trim() !== '');

    const newFiles = (this.fileList || []).filter(f => f.originFileObj instanceof File);
    const uploadParams: FileParameter[] = newFiles
      .map(f => ({
        data: f.originFileObj as File,
        fileName: f.name
      }));

    const dto = new CreateOrEditRoomRentalDto();
    dto.id = v.id ? Number(v.id) : undefined;
    dto.roomNumber = v.roomNumber !== undefined && v.roomNumber !== null ? String(v.roomNumber) : undefined;
    dto.roomType = v.roomType !== undefined && v.roomType !== null && String(v.roomType).trim() !== '' ? (Number(v.roomType) as any) : undefined;
    dto.statusRoom = v.statusRoom !== undefined && v.statusRoom !== null && String(v.statusRoom).trim() !== '' ? (Number(v.statusRoom) as any) : undefined;
    dto.price = v.price !== undefined && v.price !== null ? String(v.price) : undefined;
    dto.area = v.area !== undefined && v.area !== null ? String(v.area) : undefined;
    dto.note = v.note ?? undefined;

    const upload$ = uploadParams.length > 0 ? this.roomRentalUpload.uploadImageDescription(uploadParams) : of<string[]>([]);
    upload$.subscribe({
      next: (uploadedPaths) => {
        dto.imagesDescription = [...existingServerPaths, ...(uploadedPaths ?? [])];
        this.serviceProxy.createOrEdit(dto).subscribe({
          next: () => {
            this.notification.success('Thành công', 'Cập nhật phòng cho thuê thành công.');
            this.saved.emit();
            this.modalRef.close();
            this.saving = false;
          },
          error: (err) => {
            console.error('Error updating room rental:', err);
            this.notification.error('Lỗi', 'Không thể cập nhật phòng cho thuê. Vui lòng thử lại.');
            this.saving = false;
          }
        });
      },
      error: (err) => {
        console.error('Error uploading images:', err);
        this.notification.error('Lỗi', 'Upload ảnh mô tả thất bại. Vui lòng thử lại.');
        this.saving = false;
      }
    });
  }
}
