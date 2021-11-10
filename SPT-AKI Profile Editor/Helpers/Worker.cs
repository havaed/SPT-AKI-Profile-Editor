﻿using MahApps.Metro.Controls.Dialogs;
using SPT_AKI_Profile_Editor.Classes;
using SPT_AKI_Profile_Editor.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPT_AKI_Profile_Editor.Helpers
{
    public class Worker
    {
        public string ErrorTitle { get; set; }
        public string ErrorConfirm { get; set; }

        private IDialogCoordinator _dialogCoordinator;
        private MainWindowViewModel _viewModel;
        private ProgressDialogController progressDialog;
        private List<WorkerTask> tasks;
        private List<WorkerNotification> workerNotifications;
        private bool isBusy = false;

        public Worker(IDialogCoordinator dialogCoordinator, MainWindowViewModel viewModel)
        {
            tasks = new List<WorkerTask>();
            workerNotifications = new List<WorkerNotification>();
            _dialogCoordinator = dialogCoordinator;
            _viewModel = viewModel;
        }

        public async void AddAction(WorkerTask task)
        {
            tasks.Add(task);
            if (!isBusy)
            {
                isBusy = true;
                progressDialog = await _dialogCoordinator.ShowProgressAsync(_viewModel,
                    task.Title,
                    task.Description);
                progressDialog.SetIndeterminate();
                RunWorkerAsync();
            }
        }

        private async void RunWorkerAsync()
        {
            while (tasks.Count > 0)
            {
                progressDialog.SetTitle(tasks[0].Title);
                progressDialog.SetMessage(tasks[0].Description);
                try
                {
                    await Task.Run(() => tasks[0].Action());
                    if (tasks[0].WorkerNotification != null)
                        workerNotifications.Add(tasks[0].WorkerNotification);
                    tasks.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    if (progressDialog.IsOpen)
                        await progressDialog.CloseAsync();
                    tasks = new();
                    await ShowMessageAsync(ErrorTitle, ex.Message);
                    Logger.Log($"LoadDataWorker | {ex.Message}");
                }
            }
            if (progressDialog.IsOpen)
                await progressDialog.CloseAsync();
            while (workerNotifications.Count > 0)
            {
                await ShowMessageAsync(workerNotifications[0].NotificationTitle,
                    workerNotifications[0].NotificationDescription);
                workerNotifications.RemoveAt(0);
            }
            isBusy = false;
        }

        private async Task ShowMessageAsync(string title, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(_viewModel,
                                title,
                                message,
                                MessageDialogStyle.Affirmative,
                                new MetroDialogSettings
                                {
                                    AffirmativeButtonText = ErrorConfirm,
                                    AnimateShow = true,
                                    AnimateHide = true
                                });
        }
    }
}