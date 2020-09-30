@echo off
SETLOCAL

echo *** Building Native ClrProfiler for linux-x64 ***
set DOCKER_BUILDKIT=1
docker build -f ..\..\docker\linux-build.dockerfile --target native-linux-binary --output type=local,dest=runtimes/linux-x64/native ..\..\

ENDLOCAL