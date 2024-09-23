
resource "random_uuid" "s3_bucket_id" {}

resource "aws_s3_bucket" "my_s3_bucket" {
  bucket        = "my-bucket-${random_uuid.s3_bucket_id.result}"
  force_destroy = true
}

resource "aws_s3_bucket_lifecycle_configuration" "my_s3_bucket_lifecycle" {
  bucket = aws_s3_bucket.my_s3_bucket.bucket

  rule {
    id     = "expire_objects_after_14_days"
    status = "Enabled"

    expiration {
      days = 14
    }

    filter {
      prefix = ""
    }
  }
}
