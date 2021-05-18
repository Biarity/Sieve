import urllib.request
import time

for run in range(500):
    contents = urllib.request.urlopen("http://localhost:6500/api/posts/getall").read()
    print(contents[-50:])


#for run in range(50):
#    contents = urllib.request.urlopen("http://localhost:6500/api/posts/getallWithSieve?filters=IsNew&sorts=popularity").read()
#    print(contents[-50:])
#    time.sleep(1)

print("done baseline")
input()
