# ERP.Api

ASP.NET Core 9 Web API + SQL Server.

Endpoint Users:

- `GET  /api/users`
- `GET  /api/users/{id}`
- `POST /api/users/create`
- `POST /api/users/update`
- `POST /api/users/delete`

Tidak ada `HttpPut` dan tidak ada `HttpDelete`.
Update dan delete sama-sama menggunakan `HttpPost`.

Delete menggunakan soft delete:
- `is_active = 0`
- `deleted_at = GETDATE()`
