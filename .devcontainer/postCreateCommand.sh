#!/bin/bash

script_dir=$(dirname "$0")

sudo -E dotnet workload update

# add common dev cert on linux
# see: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl#trust-https-certificate-on-linux
dotnet tool update -g linux-dev-certs
dotnet linux-dev-certs install

# Add certificate to cert store
dotnet dev-certs https --clean  ## remove the pre-created cert and add the one from the repo
dotnet dev-certs https --check
if [ $? -ne 0 ]; then
    dotnet dev-certs https --clean --import "$script_dir/../aspnetapp.pfx" --password "password"
fi

sudo -E dotnet dev-certs https -ep /usr/local/share/ca-certificates/aspnet/https.crt --format PEM
sudo update-ca-certificates
