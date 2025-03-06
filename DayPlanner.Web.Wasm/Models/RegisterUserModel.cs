using DayPlanner.Abstractions.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Web.Wasm.Models;

internal class RegisterUserModel : RegisterUserRequest
{
    public string ConfirmPassword { get; set; } = default!;
}
