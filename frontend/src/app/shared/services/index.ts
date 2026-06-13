// NOTE:
// We re-export from the NSwag-generated monolith `service-proxies.ts` to keep
// types/imports intact (API_BASE_URL token, HttpClient deps, helpers, ...).
// The split files under this directory are kept for reference but are not
// used as the primary exports.
export * from '../service-proxies';
export * from '../contract.models';
export { ContractApiService } from '../contract-api.service';