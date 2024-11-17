#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/sysinfo.h>
#include <sys/statvfs.h>

float get_cpu_usage() {
    long double a[4], b[4];
    FILE *fp = fopen("/proc/stat", "r");
    if (fp == NULL) {
        perror("Error opening /proc/stat");
        return -1;
    }
    fscanf(fp, "cpu %Lf %Lf %Lf %Lf", &a[0], &a[1], &a[2], &a[3]);
    fclose(fp);
    sleep(1);
    fp = fopen("/proc/stat", "r");
    if (fp == NULL) {
        perror("Error opening /proc/stat");
        return -1;
    }
    fscanf(fp, "cpu %Lf %Lf %Lf %Lf", &b[0], &b[1], &b[2], &b[3]);
    fclose(fp);

    long double total_a = a[0] + a[1] + a[2] + a[3];
    long double total_b = b[0] + b[1] + b[2] + b[3];
    long double total_idle = b[3] - a[3];

    return (float)((total_b - total_a - total_idle) / (total_b - total_a) * 100.0);
}

void get_memory_usage(float *used_percent, long *total_memory_mb) {
    struct sysinfo info;
    if (sysinfo(&info) != 0) {
        perror("sysinfo error");
        *used_percent = -1;
        *total_memory_mb = -1;
        return;
    }
    *total_memory_mb = info.totalram / 1024 / 1024;
    *used_percent = ((info.totalram - info.freeram) / (float)info.totalram) * 100.0;
}

void get_disk_usage(long *used_gb, long *free_gb, long *total_gb) {
    struct statvfs stat;
    if (statvfs("/", &stat) != 0) {
        perror("statvfs error");
        *used_gb = -1;
        *free_gb = -1;
        *total_gb = -1;
        return;
    }
    *total_gb = (stat.f_blocks * stat.f_frsize) / 1024 / 1024 / 1024;
    *free_gb = (stat.f_bfree * stat.f_frsize) / 1024 / 1024 / 1024;
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
        printf("%.2f%% of %ld\n", used_percent, total_memory_mb);
    } else if (strcmp(argv[1], "disk") == 0) {
        long used_gb, free_gb, total_gb;
        get_disk_usage(&used_gb, &free_gb, &total_gb);
        printf("%ld, %ld, %ld\n", used_gb, free_gb, total_gb);
    } else {
        fprintf(stderr, "Unknown command: %s\n", argv[1]);
        return 1;
    }

    return 0;
}