# QueueConsumer

A simple consumer for Rabbit MQ sending a post with basic auth and all structure for retry and dead queue;

## Running with Docker

```
docker run --name queue-consumer -d \
    -e QueueConnectionString="amqp://user:pass@localhost:5672/current-vh" \
    -e QueueName=my-demo-queue \
    -e Url=http://pruu.herokuapp.com/dump/queue-consumer \
    -e User=user \
    -e Pass=password \
    -e TimeoutInSeconds=30 \
    -e MaxThreads=100 \
    -e PopulateQueueQuantity=10000 \
    -e RetryCount=5 \        
    -e RetryTTL=300000 \
    thiagobarradas/queue-consumer:latest
```

## Configuration

Set environment variables to setup QueueConsumer:

| Env Var | Type | Required | Description | e.g. |
| ------- | ---- | -------- | ----------- | ---- |
| `QueueConnectionString` | string | yes | rabbit connection | `amqp://user:pass@localhost:5672/current-vh` |
| `QueueName`             | string | yes | origin queue name and prefix for retry/dead queues | `some-queue` |
| `Url`                   | string | yes | url to send post with content as json body | `https://domain.com/service/v1/hook` |
| `User`                  | string | no  | basic auth username | `username` |
| `Pass`                  | string | no  | basic auth password | `password` |
| `TimeoutInSeconds`      | int    | no  | timeout to send post | `60` default |
| `MaxThreads`            | int    | no  | thread parallel max number | `20` default |
| `PopulateQueueQuantity` | int    | no  | send messages to populate main queue, `0` to disable | `0` default |
| `RetryCount `           | int    | no  | max number of retries, `0` to disable | `5` default |
| `RetryTTL`              | int    | no  | ttl in ms to retry a message, `0` to disable | `60000` default |

### Generated queues/exchanges/routing-keys

| Queue | Exchange | Routing Key |
| ----- | -------- | ----------- | 
| `{QueueName}` | `{QueueName}-exchange` | `{QueueName}-routing-key` |
| `{QueueName}-retry` | `{QueueName}-retry-exchange` | `{QueueName}-retry-routing-key` |
| `{QueueName}-dead` | `{QueueName}-dead-exchange` | `{QueueName}-dead-routing-key` |

## How can I contribute?

Please, refer to [CONTRIBUTING](.github/CONTRIBUTING.md)

## Found something strange or need a new feature?

Open a new Issue following our issue template [ISSUE TEMPLATE](.github/ISSUE_TEMPLATE.md)

## Did you like it? Please, make a donate :)

if you liked this project, please make a contribution and help to keep this and other initiatives, send me some Satochis.

BTC Wallet: `1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX`

![1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX](https://i.imgur.com/mN7ueoE.png)
