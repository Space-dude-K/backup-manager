namespace backup_manager.Model
{
    class Mail
    {
        private string email;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        private string subject;

        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }
    }
}