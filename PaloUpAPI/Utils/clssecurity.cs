using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Collections.Specialized;
using System.Security.Cryptography;
/// <summary>
/// Summary description for clssecurity
/// </summary>
public class clssecurity
{
	Byte[]  lbtVector  = {240, 3, 45, 29, 0, 76, 173, 59};
    string lscryptoKey = "ChangeThis!";


    public string psDescrypt(string squerystring)
    {
       byte[] buffeer;
       TripleDESCryptoServiceProvider loCryptoClass =new TripleDESCryptoServiceProvider();
       MD5CryptoServiceProvider loCryptoProvider= new MD5CryptoServiceProvider();
        try
        {
       
                buffeer=Convert.FromBase64String(squerystring);
                loCryptoClass.Key=loCryptoProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(lscryptoKey));
                loCryptoClass.IV = lbtVector;
                return Encoding.ASCII.GetString(loCryptoClass.CreateDecryptor().TransformFinalBlock(buffeer, 0, buffeer.Length));
        
        }
        catch(Exception e)
        {
            throw e;
        }
        finally{
            loCryptoClass.Clear();
            loCryptoProvider.Clear();
            loCryptoClass = null;
            loCryptoProvider = null;
        }
    }
   
    public string psEncrypt(string sInputval)
    {
    
        TripleDESCryptoServiceProvider loCryptoClass = new TripleDESCryptoServiceProvider();
        MD5CryptoServiceProvider    loCryptoProvider=new MD5CryptoServiceProvider();
        byte[] lbtBuffer;

        try
        {
            lbtBuffer = System.Text.Encoding.ASCII.GetBytes(sInputval);
            loCryptoClass.Key = loCryptoProvider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(lscryptoKey));
            loCryptoClass.IV = lbtVector;
            sInputval = Convert.ToBase64String(loCryptoClass.CreateEncryptor().TransformFinalBlock(lbtBuffer, 0, lbtBuffer.Length));
            return sInputval;
        }
        catch(CryptographicException ex)
        {
            throw ex;
        }
        catch(FormatException ex)
        {
        throw ex;    
        }
        catch(Exception ex)
        {
        throw ex;
        }
        finally
        {
            loCryptoClass.Clear();
            loCryptoProvider.Clear();
            loCryptoClass = null;
            loCryptoProvider = null;
        }
    
    }

}