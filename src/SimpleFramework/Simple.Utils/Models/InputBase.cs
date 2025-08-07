using System;
using System.Collections.Generic;
using System.Text;
using Simple.Utils.Models.Dto;

namespace Simple.Utils.Models
{
    public class InputBase<T> : BaseDto
    {
        public T Id { get; set; }
    }

    public class NullInputId : InputBase<Guid?>
    {
    }

    public class InputId : InputBase<Guid>
    {
    }
}