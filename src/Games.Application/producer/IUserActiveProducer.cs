using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Games.Application.producer
{
    public interface IUserActiveProducer
    {
        void PublishUserActiveEvent(UserEvent user);
    }
}
