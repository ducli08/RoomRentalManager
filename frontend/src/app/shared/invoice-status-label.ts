import { InvoiceStatus } from './services';

export function invoiceStatusLabel(status?: InvoiceStatus): string {
  switch (status as unknown as number) {
    case 1:
      return 'Nháp';
    case 2:
      return 'Đã phát hành';
    case 3:
      return 'Thanh toán một phần';
    case 4:
      return 'Đã thanh toán';
    case 5:
      return 'Đã hủy';
    default:
      return 'Không rõ';
  }
}

