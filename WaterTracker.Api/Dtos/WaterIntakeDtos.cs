using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterTracker.Api.Dtos
{
    public class WaterIntakeDtos
    {
        public record CreateWaterIntakeRequest(
            double Amount,
            DateTime Date
        );

        public record UpdateWaterIntakeRequest(
            double Amount,
            DateTime Date
        );

        public record WaterIntakeResponse(
            Guid Id,
            double Amount,
            DateTime Date,
            DateTime UpdatedAt
        );
    }
}