#!/usr/bin/env python3
"""
Test script to validate Telegram WebApp initData
Based on official Telegram documentation
"""
import hashlib
import hmac
from urllib.parse import parse_qsl

# Data from Railway logs
bot_token = "8591035047:AAH_0hYmc3PU9fG7sWg5OB8DrpYKCkT5-d0"
init_data = "user=%7B%22id%22%3A6698369098%2C%22first_name%22%3A%22LL%22%2C%22last_name%22%3A%22%22%2C%22username%22%3A%22hikill8%22%2C%22language_code%22%3A%22ru%22%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FWv2Pan8AblvcXnRiYGifRMtzn8SiynLRMmeKIyWtl75-qbOEMAuzqnhDHrtjIIC1.svg%22%7D&chat_instance=-5471737045128434439&chat_type=sender&auth_date=1765127072&signature=Wn1Xo5OboCJ7X-KwCMsfhAp6okKjKMuoKKyYoYKskC9_F7h-5wIYoIlgA86kjFIZ3FKofmWXT0aTOmEl_fweDw&hash=20fe086cb18a6481d11f8fc5e16543e8e3bf28d3aaef970997e2bedbbd7277f5"

print("=" * 80)
print("TELEGRAM WEBAPP INITDATA VALIDATION TEST")
print("=" * 80)
print()

# Parse init_data (parse_qsl automatically URL-decodes values)
parsed_data = dict(parse_qsl(init_data))
received_hash = parsed_data.pop('hash')
# ONLY hash is excluded - signature MUST be included!

print("📦 Parsed data (after removing hash and signature):")
for key in sorted(parsed_data.keys()):
    value = parsed_data[key]
    display_value = value if len(value) < 100 else value[:100] + "..."
    print(f"  {key} = {display_value}")
print()

# Create data check string (sorted by key)
data_check_string = '\n'.join([f"{k}={v}" for k, v in sorted(parsed_data.items())])

print("📝 Data check string:")
print(data_check_string)
print()

# Compute secret key: HMAC-SHA256(bot_token, "WebAppData")
secret_key = hmac.new(
    "WebAppData".encode('utf-8'),
    bot_token.encode('utf-8'),
    hashlib.sha256
).digest()

print(f"🔐 Secret key (hex): {secret_key.hex()}")
print()

# Compute hash: HMAC-SHA256(data_check_string, secret_key)
computed_hash = hmac.new(
    secret_key,
    data_check_string.encode('utf-8'),
    hashlib.sha256
).hexdigest()

print(f"✅ Computed hash: {computed_hash}")
print(f"📨 Received hash:  {received_hash}")
print()

is_valid = computed_hash == received_hash
print("=" * 80)
if is_valid:
    print("✅ VALIDATION SUCCESS!")
else:
    print("❌ VALIDATION FAILED!")
print("=" * 80)

