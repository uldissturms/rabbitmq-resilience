rabbitmq-resilience
===================

rabbitmq-resilience

This repository has been created to reflect the resiliency in rabbitmq message processing.
It compares two libraries: 
 - Custom
 - Native
As a conslusion 
 - the custom library is ack-ing the message when exception is thrown while processing message which is not acceptable and should not be used
 - the native library provides options for:
    - putting the message back
    - sending it to dead letter exchange
Dead letter exchanges are very flexible and several solutions can be created:
 - message is put at the back of the same exchange
 - message is put in a different exchange with the same routing key
 - message is put in a different exchange with a different routing key
 
Dead letter messages contain additional x-death properties that are helpful for addressing issues:
 - stop retry at certain number of retries
 - stop retry after certain timespan
 
Also x-dead-letter-exchange and x-dead-letter-routing-key properties on queues are great for diverse scenarios.
 
Detailed information can be found: http://www.rabbitmq.com/dlx.html
