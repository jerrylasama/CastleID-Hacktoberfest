import os
import mmap

print("========MP3FIX========")

f = "./c3aee919251cf4a95e02c78b1f0f0d343e7e50"

#(12*32/48000+32)*4 = 128

if not f.endswith(".mp3"):
    isFile = os.path.isfile(f)
    if isFile:
        file = open(f, "rb+")
        file.seek(0x9C)
        isMP3 = file.read(4)
        if str(isMP3) == "b'LAME'":
            print("Reading MP3 Header frame...")
            file.seek(0)
            hF = file.read(3)
            print("found 1st header : " + str(hF))
            file.close()
            with open(f, "rb+") as file:
                offset = 0
                mm = mmap.mmap(file.fileno(), 0)
                
                mm.seek(0,2)
                print("length file : " + str(mm.tell()))
                mm.seek(offset)

                
                while True:
                    mm.seek(offset+3)
                    offset = mm.find(hF)
                    if offset != -1:
                        print("Frame " + str(hex(offset)) + " FIXED")
                        mm.seek(offset)
                        mm.write(b'\xFF\xF3\x84')
                        mm.seek(offset-1)

                    else:
                        mm.close()
                        break;
