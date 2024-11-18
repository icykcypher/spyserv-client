#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <windows.h>
#include <psapi.h>

float get_cpu_usage() {
    FILETIME idleTime, kernelTime, userTime;
    ULARGE_INTEGER prevSysIdle, prevSysKernel, prevSysUser;
    ULARGE_INTEGER sysIdle, sysKernel, sysUser;

    if (GetSystemTimes(&idleTime, &kernelTime, &userTime) == 0) {
        perror("GetSystemTimes failed");
        return -1;
    }

    prevSysIdle.QuadPart = ((ULARGE_INTEGER *)&idleTime)->QuadPart;
    prevSysKernel.QuadPart = ((ULARGE_INTEGER *)&kernelTime)->QuadPart;
    prevSysUser.QuadPart = ((ULARGE_INTEGER *)&userTime)->QuadPart;

    Sleep(1000);  // Заменяет sleep(1) в Linux

    if (GetSystemTimes(&idleTime, &kernelTime, &userTime) == 0) {
        perror("GetSystemTimes failed");
        return -1;
    }

    sysIdle.QuadPart = ((ULARGE_INTEGER *)&idleTime)->QuadPart;
    sysKernel.QuadPart = ((ULARGE_INTEGER *)&kernelTime)->QuadPart;
    sysUser.QuadPart = ((ULARGE_INTEGER *)&userTime)->QuadPart;

    // Вычисляем использование процессора
    ULONGLONG sysTotal = (sysKernel.QuadPart - prevSysKernel.QuadPart) + (sysUser.QuadPart - prevSysUser.QuadPart);
    ULONGLONG sysIdleTime = sysIdle.QuadPart - prevSysIdle.QuadPart;
    
    return (float)(sysTotal - sysIdleTime) / sysTotal * 100.0;
}

void get_memory_usage(float *used_percent, long *total_memory_mb) {
    MEMORYSTATUSEX status;
    status.dwLength = sizeof(status);

    if (GlobalMemoryStatusEx(&status) == 0) {
        perror("GlobalMemoryStatusEx failed");
        *used_percent = -1;
        *total_memory_mb = -1;
        return;
    }

    *total_memory_mb = status.ullTotalPhys / 1024 / 1024;
    *used_percent = ((status.ullTotalPhys - status.ullAvailPhys) / (float)status.ullTotalPhys) * 100.0;
}

void get_disk_usage(long *used_gb, long *free_gb, long *total_gb) {
    ULARGE_INTEGER freeBytesToCaller, totalBytes, totalFreeBytes;
    if (GetDiskFreeSpaceEx("C:\\", &freeBytesToCaller, &totalBytes, &totalFreeBytes) == 0) {
        perror("GetDiskFreeSpaceEx failed");
        *used_gb = -1;
        *free_gb = -1;
        *total_gb = -1;
        return;
    }

    *total_gb = totalBytes.QuadPart / 1024 / 1024 / 1024;
    *free_gb = totalFreeBytes.QuadPart / 1024 / 1024 / 1024;
    *used_gb = *total_gb - *free_gb;
}

int main(int argc, char *argv[]) {
    if (argc < 2) {
        fprintf(stderr, "Usage: %s [cpu|memory|disk]\n", argv[0]);
        return 1;
    }

    if (strcmp(argv[1], "cpu") == 0) {
        printf("%.2f%%\n", get_cpu_usage());
    } else if (strcmp(argv[1], "memory") == 0) {
        float used_percent;
        long total_memory_mb;
        get_memory_usage(&used_percent, &total_memory_mb);
        printf("%.2f%% of %ld MB\n", used_percent, total_memory_mb);
    } else if (strcmp(argv[1], "disk") == 0) {
        long used_gb, free_gb, total_gb;
        get_disk_usage(&used_gb, &free_gb, &total_gb);
        printf("%ld GB used, %ld GB free, %ld GB total\n", used_gb, free_gb, total_gb);
    } else {
        fprintf(stderr, "Unknown command: %s\n", argv[1]);
        return 1;
    }

    return 0;
}
