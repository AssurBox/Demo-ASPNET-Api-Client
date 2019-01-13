using AssurBox.SDK.DTO;
using AssurBox.SDK.DTO.GreenCard.Car;
using Bogus;
using System;

namespace AssurBox.Samples.Client.Garage.Web.Core
{
    public static class FakeData
    {
        public static GreenCardRequestInitialization GetGreenCardRequest()
        {
            var cars = new[] {
                new { Make="Audi",Model="Audi Q3",Version="(2) 2.0 TDI 150 ULTRA BUSINESS LINE" },
                new { Make="Bmw",Model="Bmw I3",Version="PROLONGATEUR D AUTONOMIE URBANLIFE LOFT" },
                new { Make="Bmw",Model="Bmw Serie 3 Gt",Version="(F34) (2) 320D XDRIVE 190 LUXURY" },
                new { Make="Volvo",Model="Volvo Xc70 (2e Generation)",Version="II (2) D5 220 AWD MOMENTUM BUSINESS GEARTRONIC" },
                new { Make="Lancia",Model="Lancia Musa",Version="STATION WAGON 1.8 LX" },
                new { Make="Kia",Model="Kia Venga",Version="1.6 125 PREMIUM BVA" },
                new { Make="Peugeot",Model="Peugeot 2008",Version="1.4 HDI 68 BUSINESS" },
                new { Make="Fiat",Model="Fiat Ducato 3",Version="III (2) DC MAXI 3.5 L 2.3 MULTIJET 130 PACK PRO NAV" },
                new { Make="Seat",Model="Seat Toledo 4",Version="IV 1.6 TDI 90 ITECH BUSINESS" },
            };
            Randomizer r = new Randomizer();

            int carindex = r.Int(0, cars.Length - 1);


            var fakeCarDetails = new Faker<Car>().
                RuleFor(c => c.DateOfFirstRegistration, c => c.Date.Past(5))
                .RuleFor(c => c.Fuel, c => c.PickRandom<FuelTypes>())
                .RuleFor(c => c.Make, cars[carindex].Make)
                .RuleFor(c => c.Model, cars[carindex].Model)
                .RuleFor(c => c.Version, cars[carindex].Version)
                .RuleFor(c => c.CO2, c => c.Random.Int(20, 200))
                .RuleFor(c => c.EnginePowerInKw, c => c.Random.Int(75, 190))
                .RuleFor(c => c.EngineSizeInCm3, c => c.Random.Int(999, 3000))
                .RuleFor(c => c.RunningOrderMassInKg, c => c.Random.Int(955, 2020))
                .RuleFor(c => c.NumberOfFrontSeats, c => c.Random.Int(2, 3))
                .RuleFor(c => c.NumberOfBackSeats, c => c.Random.Int(2, 6))
                .RuleFor(c => c.PriceWithOption, c => c.Random.Int(20000, 95000).ToString())
                .RuleFor(c => c.PriceIncludesVAT, true)
                //.RuleFor(c=>c.UsedVehicleMileage)
                // occaz ?
                ;
            var fakeAddress = new Faker<SDK.DTO.Address>()
               .RuleFor(c => c.Number, c => c.Address.BuildingNumber())
              .RuleFor(c => c.Street, c => c.Address.StreetName())
              .RuleFor(c => c.ZipOrPostcode, c => c.Address.ZipCode())
              .RuleFor(c => c.City, c => c.Address.City())
              .RuleFor(c => c.Country, "Luxembourg");

            var fakePerson = new Faker<SDK.DTO.Person>().
               RuleFor(c => c.FirstName, c => c.Person.FirstName)
              .RuleFor(c => c.LastName, c => c.Person.LastName)
              .RuleFor(c => c.Phone, c => c.Person.Phone)
              .RuleFor(c => c.Email, c => c.Person.Email)
              .RuleFor(c => c.BirthDate, c => c.Person.DateOfBirth)
              .RuleFor(c => c.Address, fakeAddress.Generate())
              .RuleFor(c => c.DrivingLicenseNumber, c => c.Random.AlphaNumeric(13))
              ;

            var fakeCustomer = new Faker<Customer>().
               RuleFor(c => c.Person, c => fakePerson.Generate())
               ;

            var fakeRequest = new Faker<GreenCardRequestInitialization>()
                  .RuleFor(u => u.CarDetails, f => fakeCarDetails.Generate())
                  .RuleFor(u => u.VehicleOwner, f => fakeCustomer.Generate())
                  ;

            var fakeGreenCardRequestInitialization = fakeRequest.Generate();

            // fix some values
            fakeGreenCardRequestInitialization.CarDetails.MaximumMassInKg = fakeGreenCardRequestInitialization.CarDetails.RunningOrderMassInKg + r.Int(500, 1500);
            fakeGreenCardRequestInitialization.CarDetails.PriceWithoutOption = (Convert.ToInt32(fakeGreenCardRequestInitialization.CarDetails.PriceWithOption) - r.Int(500, 5000)).ToString();
            fakeGreenCardRequestInitialization.VehicleOwner.Person.NationalIdentityNumber = $"{fakeGreenCardRequestInitialization.VehicleOwner.Person.BirthDate}XXXXX";
            fakeGreenCardRequestInitialization.EffectiveDate = DateTime.Today;
            return fakeGreenCardRequestInitialization;
        }

    }
}