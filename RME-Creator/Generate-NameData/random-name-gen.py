import random
import string

def generate_random_string(length=16):
    return ''.join(random.choice(string.ascii_lowercase) for _ in range(length))

def create_random_strings(num_strings):
    random_strings = set()
    
    while len(random_strings) < num_strings:
        random_strings.add(generate_random_string())
    
    with open("random_strings.txt", "a") as file:
        for s in random_strings:
            file.write(s + "\n")

# Example usage
num_strings = 500  # You can change this value to the desired number of names
create_random_strings(num_strings)
