# MontageJobExcecutor

## Overview

TODO

## Install

* Install montage tools

```bash
sudo apt install montage
sudo apt install montage-gridtools
```

* Install dotnet:

```bash
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2
```

* Create file `montage-job-executor.service` in `/etc/systemd/system` directory

```text
[Unit]
Description=Montage Job Executor

[Service]
ExecStart=/usr/bin/dotnet /home/ubuntu/MontageJobExecutor/MontageJobExecutor.dll
WorkingDirectory=/home/ubuntu/MontageJobExecutor
User=ubuntu
Group=ubuntu
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=montage-job-executor

[Install]
WantedBy=multi-user.target
```

* Add servie to systemd

``` bash
sudo systemctl enable montage-job-executor.service
sudo systemctl start montage-job-executor.service
sudo systemctl status montage-job-executor.service (check if service is running)
```
