namespace G1.health.Shared.Utilities.Common;

public class RegistrationConsts
{
    public const string Name = "Name";

    public const string Surname = "Surname";

    public const string Email = "Email";

    public const string DateOfBirth = "Date Of Birth";

    public const string Gender = "Gender";

    public const string PhoneNumber = "Phone Number";

    public const string City = "City";

    public const string CountryCode = "Country Code";

    public const string CreationFlag = "Creation Flag";

    public const string PatientFirstName = "Child's Full Name";

    public const string PatientLastName = "Last Name";

    public const string PatientAge = "Child's Age";

    public const string Concerns = "Concerns";

    public const string TermsAndConditions = "Terms And Conditions";
    public const string Address = "Address";

    /* No Action After User Registration */
    public const int NoPostRegistrationAction = 0;

    /* Session Creation After User Registration */
    public const int PostRegistrationSessionCreation = 1;

    /* Appointment Creation After User Registration */
    public const int PostRegistrationAppointmentCreation = 2;
}