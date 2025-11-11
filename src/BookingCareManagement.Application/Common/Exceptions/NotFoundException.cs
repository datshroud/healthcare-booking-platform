using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Common.Exceptions;

// Chúng ta sẽ "ném" (throw) lỗi này khi không tìm thấy Doctor
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
