import { PaymentSubmissionStatus } from './services';

export function paymentSubmissionStatusLabel(status?: PaymentSubmissionStatus): string {
  switch (status as unknown as number) {
    case 1:
      return 'Chờ duyệt';
    case 2:
      return 'Đã duyệt';
    case 3:
      return 'Từ chối';
    default:
      return 'Không rõ';
  }
}

