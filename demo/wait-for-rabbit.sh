#!/bin/sh

while ! /etc/init.d/rabbitmq-server status | grep -m1 'is running'; do
    sleep 1
    echo "Waiting for rabbit service..."
done
