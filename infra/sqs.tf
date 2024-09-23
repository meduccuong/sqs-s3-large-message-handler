resource "random_uuid" "sqs_queue_id" {}

resource "aws_sqs_queue" "my_sqs_queue" {
  name = "my-queue-${random_uuid.sqs_queue_id.result}"
}

# Random UUID để tạo tên duy nhất cho SQS queue
