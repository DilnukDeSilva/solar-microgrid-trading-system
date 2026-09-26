# Member 3: Energy Reservations & QR Dispatch

Branch: `feature/m3-reservations` · Module owner for the whole reservation lifecycle (create → update → cancel → approve), the business rules on it, and QR generation.

The detailed implementation guide is in `docs/M3-RESERVATIONS-GUIDE.md`.

## Marks you lead
- **Individual:** Slot booking management on the web (5), **Reservation workflow & booking management (9)**, QR generation (feeds the operator flow), SQLite (shared), Web↔API (2), Mobile↔API (2).
- **Group:** DFD diagrams, references list, business-rule test evidence.

## Scope

### API (`ReservationsController`, `ReservationService`)
| Endpoint | Role | Rules |
|---|---|---|
| `POST /api/reservations` | Prosumer (own NIC) / staff on behalf | prosumer `Active`; station `Active`; slot free; **slot start within the next 7 days** (`RULE_7DAYS`); the slot is claimed atomically (`SLOT_TAKEN`) |
| `PUT /api/reservations/{id}` | owner / staff | current booking ≥ **12 h** away (`RULE_12H`); new slot also meets the 7-day rule; old slot released; an `Approved` booking goes back to `Pending` and its QR is cleared |
| `POST /api/reservations/{id}/cancel` | owner / staff | ≥ 12 h notice; only `Pending`/`Approved`; frees the slot |
| `GET /api/reservations/{id}` | owner / staff | a prosumer sees only their own (else 403) |
| `POST /api/reservations/{id}/approve` | GridOperator, Backoffice | only `Pending`, and only while still in the future; generates a random single-use `qrToken` |

### Web
Reservations page: upcoming bookings (uses M4's list endpoint), **create on behalf of a prosumer** (NIC → station → slot), edit, cancel (the "cancel with the assistance of a grid operator" scenario), approve. Show the API's rule messages in Bootstrap alerts.

### Android
Book (station → date → free slot → confirm) · My bookings → Modify / Cancel · **Summary screen after each action** (created / updated / cancelled: station, slot time, status, reference) · **QR screen** for `Approved` bookings (render `qrToken` as a QR bitmap).

### SQLite
`my_reservations(id PK, station_name, slot_start, status, qr_token, synced_at)`: the prosumer can open their QR without signal at the station. It refreshes from the API when online.

## Roadmap
| When | Do |
|---|---|
| Sat 26 PM | Rule design, service + create endpoint with atomic slot claim. |
| Sun 27 | Update, cancel, get, approve. **Merged by 20:00** with Swagger boundary tests. |
| Mon 28 | Web reservation pages. Android booking flow on M1's shell. |
| Tue 29 | Modify/cancel, summary screens, QR screen, SQLite cache. Team LAN test at 18:00. |
| Wed 30 | DFD, references, your code in the report, contribution + AI reflection, video segment. |

## Definition of done
The boundary tests in the guide all pass through IIS from both clients. The summary page is shown after every action. The QR appears only once the booking is approved.
