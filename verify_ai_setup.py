import sys
import importlib

packages = [
    ("numpy", "numpy"),
    ("pandas", "pandas"),
    ("matplotlib", "matplotlib"),
    ("scikit-learn", "sklearn"),
    ("jupyterlab", "jupyterlab"),
]

optional_packages = [
    ("torch", "torch"),
    ("tensorflow", "tensorflow"),
    ("datasets", "datasets"),
]

print("Python version:", sys.version)
print()

def check_package(package):
    display_name, module_name = package
    if importlib.util.find_spec(module_name) is not None:
        print(f"{display_name} is installed.")
    else:
        print(f"{display_name} is NOT installed.")

print("Checking required packages:")
for package in packages:
    check_package(package)

print("\nChecking optional packages:")
for package in optional_packages:
    check_package(package)

print()

try:
    import torch
    
    print("PyTorch device check:")
    if torch.cuda.is_available():
        print("CUDA is available. Using GPU.")
        print(f"GPU count: {torch.cuda.device_count()}")
        print(f"GPU name: {torch.cuda.get_device_name(0)}")
    else:
        print("CUDA is not available. Using CPU.")
except ImportError:
    print("PyTorch is not installed.")

print("\nAI setup verification complete.")