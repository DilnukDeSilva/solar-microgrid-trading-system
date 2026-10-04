/*
 * File: VerifyQrRequestDto.cs
 * Description: Body for POST /api/reservations/verify-qr. The QR contains only this opaque token.
 * Author: Herath D M S T (IT22639776)
 */

namespace SmartSolar.Api.DTOs;

public class VerifyQrRequestDto
{
    public string QrToken { get; set; } = string.Empty;
}
