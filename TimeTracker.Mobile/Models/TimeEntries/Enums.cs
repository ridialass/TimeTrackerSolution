using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Models.TimeEntries
{
    public enum WorkSessionType
    {
        Regular = 0, Night = 1, OutOfTown = 2, Extraordinary = 3, Sickness = 4, Vacation = 5, Other = 6
    }

    public enum DinnerPaidBy
    {
        None = 0, Employee = 1, Client = 2
    }

}
