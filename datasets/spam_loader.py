import pandas as pd
from pathlib import Path
from sklearn.model_selection import train_test_split
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt

# Load the dataset
url = "https://raw.githubusercontent.com/justmarkham/pycon-2016-tutorial/master/data/sms.tsv"
spam_data = pd.read_csv(url, encoding='latin-1', sep='\t', header=None, names=['label', 'message'])

print(spam_data.head())

print()
print("Dataset shape:", spam_data.shape)

print()
print("Label counts:")
print(spam_data['label'].value_counts())

spam_data["label_num"] = spam_data["label"].map({"ham": 0, "spam": 1})

X = spam_data["message"]
y = spam_data["label_num"]

X_train, X_test, y_train, y_test = train_test_split(
	X,
	y,
	test_size=0.2,
	random_state=42,
	stratify=y,
)

vectorizer = CountVectorizer()
X_train_vectorized = vectorizer.fit_transform(X_train)
X_test_vectorized = vectorizer.transform(X_test)

model = MultinomialNB()
model.fit(X_train_vectorized, y_train)

y_pred = model.predict(X_test_vectorized)

print()
print("Confusion Matrix:")
print(confusion_matrix(y_test, y_pred))

print()
print("Classification Report:")
print(classification_report(y_test, y_pred))

plt.figure(figsize=(6, 4))
plt.bar(['Ham', 'Spam'], spam_data['label_num'].value_counts(), color=['blue', 'orange'])
plt.title('Distribution of Ham and Spam Messages')
plt.xlabel('Message Type')
plt.ylabel('Count')
plt.tight_layout()
output_root = Path(__file__).resolve().parents[1]
plt.savefig(output_root / 'notes' / 'spam_distribution.png', dpi=150)
plt.show()
