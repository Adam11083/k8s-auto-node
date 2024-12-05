using System.Text;
using Amazon.EC2;

namespace DemoApplication.Core.Constants;

public static class Ec2LaunchConstant
{
    public const string ImageId = "ami-001f2488b35ca8aad";
    public const string KeyName = "my-k8s-pair";
    public const string SecurityGroupIds = "sg-021e0178e812ebbe7";

    private const string RawWorkerNodeScript = """
                                               #!/bin/bash
                                               sudo apt update && sudo apt install -y apt-transport-https curl
                                               #2
                                               curl https://mirrors.aliyun.com/kubernetes/apt/doc/apt-key.gpg | sudo apt-key add 
                                               #3
                                               cat <<EOF | sudo tee /etc/apt/sources.list.d/kubernetes.list
                                                   deb https://mirrors.aliyun.com/kubernetes/apt kubernetes-xenial main
                                               EOF
                                               #4
                                               yes | apt-get install docker.io
                                               #5
                                               systemctl enable docker
                                               systemctl start docker
                                               #6
                                               sudo apt update && sudo apt install -y kubelet kubeadm kubectl
                                               sudo apt-mark hold kubelet kubeadm kubectl
                                               #7
                                               kubeadm join 172.31.12.53:6443 --token 1b0b1h.o5nbpjlzmwsi4dyp \
                                               --discovery-token-ca-cert-hash sha256:e27ff8d72f3e9969c17ce79e16e14b01a475233d755051441bd89fbef98d5e1d
                                               """;

    // Convert the string to a byte array
    private static readonly byte[] ByteArray = Encoding.UTF8.GetBytes(RawWorkerNodeScript);

    // Convert the byte array to a Base64 string
    public static readonly string Base64WorkerNodeScript = Convert.ToBase64String(ByteArray);

    public static readonly InstanceType InstanceType = InstanceType.T2Medium;
}