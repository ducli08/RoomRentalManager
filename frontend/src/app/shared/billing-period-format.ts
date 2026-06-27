export function formatBillingPeriod(
  periodStart?: Date | string | null,
  periodEnd?: Date | string | null
): string {
  if (!periodStart || !periodEnd) return '-';
  const start = new Date(periodStart);
  const end = new Date(periodEnd);
  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return '-';
  const fmt = (d: Date) =>
    `${String(d.getDate()).padStart(2, '0')}/${String(d.getMonth() + 1).padStart(2, '0')}/${d.getFullYear()}`;
  return `${fmt(start)} – ${fmt(end)}`;
}
