/**
 * Sửa lỗi ghép API base + URL đầy đủ thiếu dấu / (vd: https://localhost:7246https://res.cloudinary.com/...).
 */
export function unwrapDoubleSchemeUrl(url: string): string {
  const s = url.trim();
  const m = s.match(/^https?:\/\/[^/?#]+(?=https?:\/\/)/i);
  if (m) {
    return s.slice(m[0].length);
  }
  return s;
}

/**
 * URL hiển thị cho media lưu trên CDN (Cloudinary, …) hoặc path tương đối API.
 * URL tuyệt đối (http/https) hoặc protocol-relative (//…) giữ nguyên; path tương đối ghép với API base.
 * blob:/data: không đụng tới.
 */
export function resolveAssetUrl(
  baseUrl: string | undefined | null,
  pathOrUrl: string | undefined | null
): string {
  if (pathOrUrl == null || pathOrUrl === '') {
    return '';
  }
  const s = pathOrUrl.trim();
  let out: string;
  if (/^https?:\/\//i.test(s) || s.startsWith('//')) {
    out = s;
  } else if (s.startsWith('blob:') || s.startsWith('data:')) {
    out = s;
  } else {
    out = (baseUrl ?? '') + s;
  }
  return unwrapDoubleSchemeUrl(out);
}
