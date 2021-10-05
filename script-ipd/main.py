
from requests import *
from bs4 import BeautifulSoup
from Crypto.Cipher import PKCS1_v1_5
from Crypto.PublicKey import RSA
import base64
import configparser
import threading
import time


def doFRS(username, password, datMK, index):
    session = Session()

    # Encrypt data login yang dibutuhkan
    pubkey = open('pubkey.pem', 'r').read()
    rsa_key = RSA.importKey(pubkey)
    cipher = PKCS1_v1_5.new(rsa_key)
    dataLogin = base64.b64encode(cipher.encrypt(
        username.encode() + b"|||" + password.encode()))

    # Attempt login integra
    LOGIN_INTEGRA = 'https://integra.its.ac.id/index.php'
    page = session.get(LOGIN_INTEGRA)
    time.sleep(1)
    page = session.post(LOGIN_INTEGRA, data={
                        'content': dataLogin, 'p': '', 'n': ''})
    time.sleep(1)
    page = session.get('http://integra.its.ac.id/dashboard.php')
    time.sleep(1)
    if username in page.text:
        print("Login Sukses")
        #page = session.get('https://integra.its.ac.id/dashboard.php?sim=AKAD__-__')
        page = session.get(
            'https://integra.its.ac.id/dashboard.php?sim=AKAD__10__')
        time.sleep(1)
        mulai = page.text.find("URL=")
        akademik = page.text[mulai+4:-2]
        # print(akademik)
        page = session.get(akademik)
        time.sleep(1)
        print('Masuk SIM AKADEMIK')

        print("------------------------------")
        print("Mencoba Matkul {}".format(index))
        ambilFRS = {
            'nrp': username,
            'act': "ambil",
            'key': datMK
        }
        print(ambilFRS)
        #page = session.get('https://akademik.its.ac.id/list_frs.php')
        # print(page.text)
        page = session.post(
            'https://akademik.its.ac.id/list_frs.php', data=ambilFRS)
        soup = BeautifulSoup(page.text, 'html.parser')
        redgagal = "#B30000"
        greensucc = "#006A00"
        info = "nothing"
        if redgagal in page.text:
            info = soup.find('font', attrs={'color': "#B30000"}).get_text()

        if greensucc in page.text:
            info = soup.find('font', attrs={'color': "#006A00"}).get_text()

        print("Matkul {} -> {}".format(index, info))
    else:
        print("Login Gagal")
        exit()


if __name__ == '__main__':
    config = configparser.ConfigParser()
    config.read('config.ini')

    username = config['USER']['USERNAME']
    password = config['USER']['PASSWORD']

    threads = []
    for i in range(int(config['MATKUL']['N'])):
        time.sleep(1)
        t = threading.Thread(target=doFRS, args=(
            username, password, config['MATKUL']['MK'+str(i)], i+1,))
        threads.append(t)
        t.start()
