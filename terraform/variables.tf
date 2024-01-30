variable "login" {
  type        = string
  description = "The login to use for the database."
  default     = "Adm1n157r470r"
}

variable "password" {
  type        = string
  description = "Cannot contain all or part of the login name. Should be a minimum of 8 characters, and contain upper and lower case letters, one number and one special character."
  default     = "4-v3ry-53cr37-p455w0rd"
}
