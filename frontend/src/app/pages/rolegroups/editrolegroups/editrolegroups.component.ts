import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Inject, OnInit, Output } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateOrEditRoleGroupDto, PermissionDto, RoleDto, SelectListItem, ServiceProxy } from "../../../shared/services";
import { CategoryCacheService } from "../../../shared/category-cache.service";
import { SelectListItemService } from "../../../shared/get-select-list-item.service";
import { NZ_MODAL_DATA } from "ng-zorro-antd/modal";
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormControlComponent, NzFormItemComponent, NzFormLabelComponent } from 'ng-zorro-antd/form';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { FlatTreeControl } from '@angular/cdk/tree';
import { NzTreeFlatDataSource, NzTreeFlattener, NzTreeViewModule } from 'ng-zorro-antd/tree-view';
import { take, tap } from 'rxjs';
interface TreeNode {
    name: string;
    disabled?: boolean;
    children?: TreeNode[];
    id?: number;
}

const TREE_DATA: TreeNode[] = [];

interface FlatNode {
    expandable: boolean;
    name: string;
    level: number;
    disabled?: boolean;
    id?: number;
    key: string;
}
@Component({
    selector: 'app-editrolegroups',
    imports: [NzButtonModule, NzFormItemComponent, NzFormLabelComponent,ReactiveFormsModule, CommonModule,
        NzFormControlComponent, NzModalModule, NzInputModule, NzIconModule, NzSwitchModule, NzTreeViewModule],
    templateUrl: './editrolegroups.component.html',
    styleUrl: './editrolegroups.component.css'
})
export class EditRoleGroupsComponent implements OnInit {
    isLoading = false;
    editRoleGroupForm: FormGroup;
    controlRequestArray: Array<{
            label: string;
            key: string;
            type: string;
            options?: () => SelectListItem[];
            placeholder?: string;
            validators?: any[];
        }> = [];
    @Output() saved = new EventEmitter<void>();
    private transformer = (node: TreeNode, level: number): FlatNode => {
        const existingNode = this.nestedNodeMap.get(node);
        const flatNode = existingNode && existingNode.name === node.name
            ? existingNode
            : {
                expandable: !!node.children && node.children.length > 0,
                name: node.name,
                level,
                disabled: !!node.disabled,
                id: node.id,
                key: ''
            };
        flatNode.id = node.id;
        flatNode.key = this.buildNodeKey(flatNode);
        this.flatNodeMap.set(flatNode, node);
        this.nestedNodeMap.set(node, flatNode);
        return flatNode;
    }
    flatNodeMap = new Map<FlatNode, TreeNode>();
    nestedNodeMap = new Map<TreeNode, FlatNode>();
    /**
     * Use stable keys (role:<id>, perm:<id>) instead of FlatNode identity.
     * Flat nodes can be recreated during flatten/render; object-identity selection causes "needs 2 clicks".
     */
    checkedKeys = new Set<string>();
    private pendingPermissionIds: number[] | null = null;
    treeControl = new FlatTreeControl<FlatNode>(
        node => node.level,
        node => node.expandable
    );

    treeFlattener = new NzTreeFlattener(
        this.transformer,
        node => node.level,
        node => node.expandable,
        node => node.children
    );

    dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);
    constructor(private fb: FormBuilder, private serviceProxy: ServiceProxy, private memoryCache: CategoryCacheService,
        private _getSelectListItem: SelectListItemService, @Inject(NZ_MODAL_DATA) public data: { roleGroupData: any }) {
        // Initialize form with basic structure first
        this.editRoleGroupForm = this.fb.group({
            id: [''],
            name: ['', Validators.required],
            descriptions: ['', Validators.required],
            active: [true],
            creatorUser: [''],
            lastUpdateUser: [''],
            createdAt: [''],
            updatedAt: ['']
        });
    }
    hasChild = (_: number, node: FlatNode): boolean => node.expandable;

    isChecked(node: FlatNode): boolean {
        return this.checkedKeys.has(node.key);
    }

    descendantsAllSelected(node: FlatNode): boolean {
        const descendants = this.treeControl.getDescendants(node);
        return descendants.length > 0 && descendants.every(child => this.isChecked(child));
    }
    descendantsPartiallySelected(node: FlatNode): boolean {
        const descendants = this.treeControl.getDescendants(node);
        const result = descendants.some(child => this.isChecked(child));
        return result && !this.descendantsAllSelected(node);
    }

    leafItemSelectionToggle(node: FlatNode): void {
        this.toggleChecked(node);
        this.checkAllParentsSelection(node);
    }

    itemSelectionToggle(node: FlatNode): void {
        const descendants = this.treeControl.getDescendants(node);
        const shouldCheck = !this.isChecked(node);
        this.setChecked(node, shouldCheck);
        descendants.forEach(child => this.setChecked(child, shouldCheck));
        this.checkAllParentsSelection(node);
    }

    checkAllParentsSelection(node: FlatNode): void {
        let parent: FlatNode | null = this.getParentNode(node);
        while (parent) {
            this.checkRootNodeSelection(parent);
            parent = this.getParentNode(parent);
        }
    }

    checkRootNodeSelection(node: FlatNode): void {
        const nodeSelected = this.isChecked(node);
        const descendants = this.treeControl.getDescendants(node);
        const descAllSelected =
            descendants.length > 0 && descendants.every(child => this.isChecked(child));
        if (nodeSelected && !descAllSelected) {
            this.setChecked(node, false);
        } else if (!nodeSelected && descAllSelected) {
            this.setChecked(node, true);
        }
    }

    getParentNode(node: FlatNode): FlatNode | null {
        const currentLevel = node.level;

        if (currentLevel < 1) {
            return null;
        }

        const startIndex = this.treeControl.dataNodes.indexOf(node) - 1;

        for (let i = startIndex; i >= 0; i--) {
            const currentNode = this.treeControl.dataNodes[i];

            if (currentNode.level < currentLevel) {
                return currentNode;
            }
        }
        return null;
    }

    ngOnInit(): void {
        this.initializeFormControls();
        if(this.data && this.data.roleGroupData) {
            const roleTreeData = localStorage.getItem('role_tree_data');
            if(roleTreeData){
                this.dataSource.setData(roleTreeData ? JSON.parse(roleTreeData) : TREE_DATA);
            }
            else{
                this.serviceProxy.getAllRole().pipe(
                            tap(result => {
                                this.onMapRolesToTree(result);
                            })
                        ).subscribe();
            }
            this.serviceProxy.getActivePermission(this.data.roleGroupData.id).subscribe((permissions: number[]) => {
                this.pendingPermissionIds = permissions;
                // wait until the tree has flattened nodes, then apply by stable id-based keys
                this.tryApplyPendingPermissions();
            });
            this.editRoleGroupForm.patchValue(this.data.roleGroupData);
        }
    }

    onMapRolesToTree(roles: RoleDto[]): void {
            // Giả sử lstUser là mảng các quyền đã lấy từ API
           const treeData: TreeNode[] = roles.map(role => {
                const node: TreeNode = {
                    id: role.id || 0,
                    name: role.name || '',
                    disabled: false,
                    children: role.permissions?.map(permission => ({
                        name: permission.name || '',
                        id: permission.id || 0,
                        disabled: false
                    })) || []
                };
                return node;
            });
            this.dataSource.setData(treeData);
            localStorage.setItem('role_tree_data', JSON.stringify(treeData));
            this.tryApplyPendingPermissions();
        }

    onSubmit(): void {
        if (!this.editRoleGroupForm.valid || this.data?.roleGroupData?.id == null) {
            return;
        }
        const roleGroupDto: CreateOrEditRoleGroupDto = this.editRoleGroupForm.value;
        const selectedFlat = this.treeControl.dataNodes.filter(n => this.isChecked(n));

        const roleMap = new Map<number, { roleName: string; permissions: PermissionDto[] }>();

        selectedFlat.forEach(node => {
            const treeNode = this.flatNodeMap.get(node);
            if (!treeNode) return;

            if (node.expandable) {
                const descendants = this.treeControl.getDescendants(node)
                    .filter(d => !d.expandable);
                const permissions = descendants
                    .map(d => this.flatNodeMap.get(d))
                    .filter((n): n is TreeNode => !!n)
                    .map(p => new PermissionDto({ id: p.id || 0, name: p.name }));

                const roleId = treeNode.id || 0;
                roleMap.set(roleId, { roleName: treeNode.name, permissions });
            } else {
                let parent = this.getParentNode(node);
                let roleId: number;
                let roleName: string;

                if (parent) {
                    const parentTree = this.flatNodeMap.get(parent);
                    roleId = parentTree?.id || 0;
                    roleName = parentTree?.name || '';
                } else {
                    roleId = treeNode.id || 0;
                    roleName = treeNode.name;
                }

                const existing = roleMap.get(roleId);
                const perm = new PermissionDto({ id: treeNode.id || 0, name: treeNode.name });
                if (existing) {
                    if (!existing.permissions.some(p => p.id === perm.id)) {
                        existing.permissions.push(perm);
                    }
                } else {
                    roleMap.set(roleId, { roleName, permissions: [perm] });
                }
            }
        });

        const roleDtos: RoleDto[] = [];
        roleMap.forEach((value, key) => {
            const uniquePermissions = Array.from(
                new Map(value.permissions.map(p => [p.id, p])).values()
            );
            const roleDto = new RoleDto({ id: key, name: value.roleName, permissions: uniquePermissions });
            roleDtos.push(roleDto);
        });

        roleGroupDto.id = this.data.roleGroupData.id;
        roleGroupDto.roleDtos = roleDtos;
        this.serviceProxy.createOrEditRoleGroup(roleGroupDto).subscribe({
            next: () => this.saved.emit(),
            error: (err) => console.error('Error updating role group:', err),
        });
    }

    initializeFormControls(): void {
        // Định nghĩa các field cho form tạo mới
        this.controlRequestArray = [
            {
                label: 'Tên nhóm quyền',
                key: 'name',
                type: 'text',
                placeholder: 'Nhập tên nhóm quyền'
            },
            {
                label: 'Trạng thái',
                key: 'active',
                type: 'bool',
                placeholder: 'Chọn trạng thái',
                validators: []
            },
            {
                label: "Nhóm quyền",
                key: 'roleDtos',
                type: 'tree',
                placeholder: 'Chọn nhóm quyền',
            },
            {
                label: 'Mô tả',
                key: 'descriptions',
                type: 'text',
                placeholder: 'Nhập mô tả',
            }
        ];

        // Tạo FormGroup động dựa trên controlRequestArray
        const formControls: { [key: string]: any } = {};
        this.controlRequestArray.forEach(control => {
            formControls[control.key] = ['', control.validators || []];
        });

        this.editRoleGroupForm = this.fb.group(formControls);
    }

    private buildNodeKey(node: FlatNode): string {
        const idPart = node.id != null ? String(node.id) : node.name;
        return node.expandable ? `role:${idPart}` : `perm:${idPart}`;
    }

    private setChecked(node: FlatNode, checked: boolean): void {
        if (checked) this.checkedKeys.add(node.key);
        else this.checkedKeys.delete(node.key);
    }

    private toggleChecked(node: FlatNode): void {
        this.setChecked(node, !this.isChecked(node));
    }

    private tryApplyPendingPermissions(): void {
        if (!this.pendingPermissionIds || this.pendingPermissionIds.length === 0) return;
        if (!this.treeControl.dataNodes || this.treeControl.dataNodes.length === 0) {
            // Defer to next tick; the tree flattener populates dataNodes asynchronously after setData().
            setTimeout(() => this.tryApplyPendingPermissions(), 0);
            return;
        }
        const permissionIds = this.pendingPermissionIds;
        this.pendingPermissionIds = null;

        // Select matching permission leaf nodes, then update parents.
        this.treeControl.dataNodes.forEach(node => {
            if (!node.expandable && node.id != null && permissionIds.includes(node.id)) {
                this.setChecked(node, true);
                this.checkAllParentsSelection(node);
            }
        });

        // Ensure parent roles become checked/indeterminate correctly.
        this.treeControl.dataNodes
            .filter(n => n.expandable)
            .forEach(n => this.checkRootNodeSelection(n));
    }
}