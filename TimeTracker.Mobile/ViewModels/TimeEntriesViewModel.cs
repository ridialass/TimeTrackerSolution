using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Resources.Strings;
using System;
using System.Collections.Generic;

namespace TimeTracker.Mobile.ViewModels;

public partial class TimeEntriesViewModel : BaseViewModel
{
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IAuthService _authService;
    private readonly ISecureStorageService _secureStore;

    private const int PageSize = 10;
    private int currentPage = 1;
    private List<TimeEntryDto> allEntries = new();

    private ObservableCollection<TimeEntryDto> timeEntries = new();
    public ObservableCollection<TimeEntryDto> TimeEntries
    {
        get => timeEntries;
        set => SetProperty(ref timeEntries, value);
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    private bool isEmpty;
    public bool IsEmpty
    {
        get => isEmpty;
        set => SetProperty(ref isEmpty, value);
    }

    private bool canLoadMore;
    public bool CanLoadMore
    {
        get => canLoadMore;
        set => SetProperty(ref canLoadMore, value);
    }

    // Utilisation de la propriété ErrorMessage du parent (BaseViewModel)
    // On ne redéfinit PAS ErrorMessage ici

    public TimeEntriesViewModel(
        IMobileTimeEntryService timeEntryService,
        IAuthService authService,
        ISecureStorageService secureStore
    )
    {
        _timeEntryService = timeEntryService;
        _authService = authService;
        _secureStore = secureStore;

        LoadTimeEntriesCommand = new AsyncRelayCommand(LoadTimeEntriesAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
    }

    public IAsyncRelayCommand LoadTimeEntriesCommand { get; }
    public IAsyncRelayCommand LoadMoreCommand { get; }

    private static string CacheKey => "TimeEntriesHistoryCache";

    public async Task LoadTimeEntriesAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        IsEmpty = false;
        TimeEntries.Clear();
        allEntries.Clear();
        currentPage = 1;
        CanLoadMore = false;

        // 1. Charger depuis le cache local si présent
        var cached = await _secureStore.GetAsync(CacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            try
            {
                var cachedEntries = System.Text.Json.JsonSerializer.Deserialize<List<TimeEntryDto>>(cached)
                                   ?? new List<TimeEntryDto>();
                if (cachedEntries.Count > 0)
                {
                    allEntries = cachedEntries.Where(e => e.EndTime != null).OrderByDescending(e => e.EndTime).ToList();
                    DisplayPage(1);
                }
            }
            catch
            {
                // Si le cache est corrompu, on l'ignore
            }
        }

        // 2. Charger depuis l'API (asynchrone)
        try
        {
            var user = _authService.CurrentUser;
            if (user == null)
            {
                ErrorMessage = AppResources.TimeEntries_NotAuthenticated;
                IsEmpty = true;
                return;
            }

            var entries = await _timeEntryService.GetTimeEntriesAsync(user.Id);
            var finishedEntries = entries.Where(e => e.EndTime != null).OrderByDescending(e => e.EndTime).ToList();
            allEntries = finishedEntries;

            // Met à jour le cache local
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(allEntries);
                await _secureStore.SetAsync(CacheKey, json);
            }
            catch { /* ignore, le cache n'est pas critique */ }

            DisplayPage(1, reset: true);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Erreur réseau." + "\n" + ex.Message;
            if (TimeEntries.Count == 0)
                IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void DisplayPage(int page, bool reset = false)
    {
        if (reset)
            TimeEntries.Clear();

        var skip = (page - 1) * PageSize;
        var pageEntries = allEntries.Skip(skip).Take(PageSize).ToList();
        foreach (var entry in pageEntries)
        {
            if (!TimeEntries.Contains(entry))
                TimeEntries.Add(entry);
        }
        currentPage = page;
        CanLoadMore = allEntries.Count > currentPage * PageSize;
        IsEmpty = (TimeEntries.Count == 0 && !IsLoading && string.IsNullOrEmpty(ErrorMessage));
    }

    public async Task LoadMoreAsync()
    {
        if (!CanLoadMore || IsLoading) return;
        IsLoading = true;
        try
        {
            DisplayPage(currentPage + 1);
        }
        finally
        {
            IsLoading = false;
        }
    }
}