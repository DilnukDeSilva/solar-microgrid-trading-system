/*
 * File: VerifyQrRequestDto.cs
 * Description: Body for POST /api/reservations/verify-qr. The QR contains only this opaque token.
 * Author: samudith
 * Created: 29/09/2026
 */

namespace SmartSolar.Api.DTOs;

public class VerifyQrRequestDto
{
    public string QrToken { get; set; } = string.Empty;
}
