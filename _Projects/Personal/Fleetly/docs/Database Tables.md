### Driver
UUID
FirstName
MiddleName
LastNames
PersonalEmail
PersonalPhoneNumber
Email
PhoneNumberId
NationalIdNumber
HiredDate
FiredDate
CompanyId
ContractTypeId
ShiftId
ManagerId
DefaultVehicleId
FleetId
Nationality
BaseId
DateOfBirth
DrivingLicenseId
Status
Rating
IsManager
IBAN
### Company
UUID
NIF
CNAE
Name
Address
ZIPCode
PhoneNumber
IsActive
LastActiveDate
RegistrationDate

`[Uber, Cabify, Freenow, Bolt, Lyft]`
### PhoneNumber
UUID
ICCNumber
IMSINumber
PhoneNumber
Pin
Puk
Status
DataPlan
CompanyId
### PhoneDevice
UUID
PhoneNumberId
IMEINumber
DriverId
CompanyId
Status
OSversion
AppVersion
PurchaseDate
DecomissionDate
### ContractType
UUID
Name
HoursPerWeek
Duration
IsPermanent
IsFulltime
CompanyId

`[permanent, fixed-term ,temporary, part-time]`

### Shift
UUID
Name
ShiftBlockId
HoursPerWeek
DaysOffId
StartTime
EndTime
LocationId
### ShiftBlock
UUID
Name
StartTime
EndTime

`[morning, afternoon, night, weekends]`
### DaysOff
UUID
Name
FirstDayOff
SecondDayOff

`[mon-tue, tue-wed, wed-thu, thu-fri, fri-sat, sat-sun, sun-mon]`
### Holidays
UUID
Name
Date
IsWorkingDay
LocationId
### Location
CountryId
RegionId
ProvinceId
CityId

`4 tables => Country, Region, Province, City`
### Manager
UUID
Name
CompanyId
BaseId
DriverId
LocationId
Status
### Base
UUID
Name
LocationId
Status
### Fleet
UUID
Name
LocationId
BaseId
ManagerId
CompanyId
IsActive
### DrivingLicense
UUID
LicenseNumber
IssuedDate
IssuingCountry
RenewalDate
ExpirationDate
DrivingLicenseType
Points
