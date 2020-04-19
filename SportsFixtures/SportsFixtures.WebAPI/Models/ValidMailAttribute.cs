using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;


namespace ObligatorioDA2.WebAPI.Models
{
    [ExcludeFromCodeCoverage]
    public class ValidMailAttribute: ValidationAttribute
    {
        

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string email =((UserModelIn)validationContext.ObjectInstance).Email;
            
            ValidationResult result;

            try
            {
                MailAddress m = new MailAddress(email);
                result = ValidationResult.Success;
            }
            catch (FormatException)
            {
                result = new ValidationResult(GetErrorMessage());
            }
            catch (ArgumentException) {
                result = new ValidationResult(GetErrorMessage());
            }

            return result;
        }

        private string GetErrorMessage()
        {
            return "Email is not valid";
        }
    }
}
