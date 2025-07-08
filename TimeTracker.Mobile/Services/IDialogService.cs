﻿using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services;

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel);
}