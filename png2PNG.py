import os

print("========png to PNG========")

path = "./"

for path, subdirs, files in os.walk(path):
    for f in files:
        curPath = os.path.join(path, f)

        if not f.endswith(".png"):
            isFile = os.path.isfile(curPath)
            if isFile:
                file = open(curPath, "rb+")
                file.seek(1)
                isPNG = file.read(3)
                if str(isPNG) == "b'png'":
                    print("[C] converting " + f + " to working PNG file.")
                    # go back to begining of byte
                    file.seek(1)
                    # replacing png to PNG
                    file.write(b"PNG")
                    # update the isPNG variable
                    file.seek(1)
                    isPNG = file.read(3)
                # closing the file
                file.close()
                # check again if the file successfuly converted then rename the file
                if str(isPNG) == "b'PNG'":
                    os.rename(curPath, curPath + ".png")
                    print("[C] successfuly convert the file.")
        else:
            print("[F] " + f + " already converted.")
