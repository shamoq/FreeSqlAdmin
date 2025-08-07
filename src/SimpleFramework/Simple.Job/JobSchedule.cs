﻿using FluentScheduler;

namespace Simple.Job
{
    public class JobSchedule
    {
        private readonly bool IsInited = false;

        public JobSchedule(params Registry[] registrys)
        {
            if (registrys.Length == 0)
            {
                registrys = new Registry[] { new JobRegistry() };
            }
            JobManager.Initialize(registrys);
            JobManager.JobEnd += JobManager_JobEnd;
            JobManager.JobStart += JobManager_JobStart;
            IsInited = true;
        }

        public event Action<JobEndInfo> JobEnd;

        public event Action<JobStartInfo> JobStart;

        private void JobManager_JobStart(JobStartInfo info)
        {
            Console.WriteLine(IsInited);
            JobStart?.Invoke(info);
        }

        private void JobManager_JobEnd(JobEndInfo obj)
        {
            JobEnd?.Invoke(obj);
        }

        public void Start()
        {
            JobManager.Start();
        }

        public void Stop()
        {
            JobManager.Stop();
        }
    }
}