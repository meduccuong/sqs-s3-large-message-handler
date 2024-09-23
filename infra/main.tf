provider "aws" {
  region = "ap-southeast-2"

  default_tags {
    tags = {
      Project = "sqs-s3-large-message"
      Owner   = "cuong.me"
    }
  }
}
