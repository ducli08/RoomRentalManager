import { UtilityReadingStatus } from './utility-reading.models';

export function utilityReadingStatusLabel(status?: UtilityReadingStatus): string {
  switch (status) {
    case UtilityReadingStatus.Confirmed:
      return 'Đã xác nhận';
    case UtilityReadingStatus.InvoiceGenerated:
      return 'Đã tạo hóa đơn';
    default:
      return '';
  }
}
