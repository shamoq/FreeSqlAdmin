﻿using FluentScheduler;
using Microsoft.IdentityModel.Tokens;
using Simple.AspNetCore;
using Simple.AspNetCore.Services;
using Simple.Job;
using Simple.Utils.Extensions;

await SimpleHost.SimpleRunConsoleAsync(args, afterBuilder: () =>
{
    var jobmanager = new JobSchedule();
    jobmanager.JobEnd += Jobmanager_JobEnd;
    jobmanager.Start();
});

static async void Jobmanager_JobEnd(FluentScheduler.JobEndInfo obj)
{
    ConsoleHelper.Debug($"执行了一次 {obj.Name} 任务");
    if (obj.Name.IsNullOrEmpty()) return;
    await Task.CompletedTask;
    // var jobInfo = new Simple.AdminApplication.Entitys.SysJobLog
    // {
    //     Name = obj.Name,
    //     StartTime = obj.StartTime,
    //     NextRun = obj.NextRun,
    //     Duration = $"{obj.Duration.Hours}时{obj.Duration.Minutes}分{obj.Duration.Seconds}秒"
    // };
    // await AppDomainEventDispatcher.PublishEvent(jobInfo);
}