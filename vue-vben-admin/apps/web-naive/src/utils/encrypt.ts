import CryptoJS from 'crypto-js';

// 明文key/iv
const AES_KEY_STR = "5xL&j2D9P0kfR#eG";
const AES_IV_STR = "aQ3@N6mFz7BvY*4H";

// base64 key/iv
export const AES_KEY_BASE64 = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(AES_KEY_STR));
export const AES_IV_BASE64 = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(AES_IV_STR));

/**
 * AES加密
 * @param plainText 明文
 * @returns 密文
 */
export function encryptByAES(plainText: string) {
  const key = CryptoJS.enc.Base64.parse(AES_KEY_BASE64);
  const iv = CryptoJS.enc.Base64.parse(AES_IV_BASE64);
  const encrypted = CryptoJS.AES.encrypt(plainText, key, {
    iv: iv,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.Pkcs7
  });
  return encrypted.toString();
}

/**
 * AES解密
 * @param cipherText 密文
 * @returns 明文
 */
export function decryptByAES(cipherText: string) {
  const key = CryptoJS.enc.Base64.parse(AES_KEY_BASE64);
  const iv = CryptoJS.enc.Base64.parse(AES_IV_BASE64);
  const decrypted = CryptoJS.AES.decrypt(cipherText, key, {
    iv: iv,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.Pkcs7
  });
  return decrypted.toString(CryptoJS.enc.Utf8);
}
