import re
import io
from decimal import Decimal
import matplotlib.pyplot as plt
datas = io.open("answer_data_pca.txt", 'r', encoding="utf-8")
#datas = io.open("answer_data_lda.txt", 'r', encoding="utf-8")
data = datas.read()
datas.close()
data = re.sub("\,", ".", data)
data = re.sub("[ ]+", "", data).split('\n')
data = [g.split(";") for g in data]

data0 = [float(g[0]) for g in data]
data1 = [float(g[1]) for g in data]
data2 = [int(g[2]) for g in data]

plt.scatter(data0, data1, c=data2, edgecolor="none", alpha=0.8, cmap=plt.cm.get_cmap("viridis", 6))

plt.xlabel("X-axis (Major Vector)")
plt.ylabel("Y-axis (Second Major Vector)")
plt.colorbar()
plt.show()
