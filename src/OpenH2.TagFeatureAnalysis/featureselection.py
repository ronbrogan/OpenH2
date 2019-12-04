# Load libraries
from sklearn.datasets import load_iris
from sklearn.feature_selection import SelectKBest
from sklearn.feature_selection import chi2
from featuredata import X,y
import numpy as np



def printArr(arr):
    for x in range(len(arr)):
        print(str(x) + " " + str(arr[x]))

def main():
    stats,pval = chi2(X,y)

    # Select two features with highest chi-squared statistics
    chi2_selector = SelectKBest(chi2, k=2)
    X_kbest = chi2_selector.fit_transform(X, y)
    
    print("stats")
    printArr(np.around(stats, decimals=2))
    print("pval")
    printArr(np.around(pval, decimals=2))
    
    # # Show results
    # print('Original number of features:', X.shape[1])
    # print('Reduced number of features:', X_kbest.shape[1])

main()