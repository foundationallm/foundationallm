set -euox pipefail
apt update
apt-get update
apt-get install -y certbot
apt-get install -y python3-pip
python3 -m pip install pip --upgrade
pip install pyopenssl --upgrade --root-user-action=ignore
pip3 install certbot certbot-dns-azure --upgrade --root-user-action=ignore