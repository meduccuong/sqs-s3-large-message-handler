output "bucket_name" {
  value = aws_s3_bucket.my_s3_bucket.bucket
}

output "queue_name" {
  value = aws_sqs_queue.my_sqs_queue.name
}
