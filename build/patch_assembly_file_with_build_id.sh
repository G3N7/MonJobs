PATH_TO_ASSEMBLY_FILE=$1
BUILD_NUMBER=$2

#sed -e "s/AssemblyVersion\(\"\d.\d.\d.\d\"\)/\AssemblyVersion\(\"\d.\d.\d.$(BUILD_NUMBER)\"\)/" ${PATH_TO_ASSEMBLY_FILE}

MATCH='AssemblyVersion("\([0-9]\).\([0-9]\).\([0-9]\).[0-9]")'
REPLACEMENT="AssemblyVersion(\"\1.\2.\3.${BUILD_NUMBER}\")"

sed -e "s/${MATCH}/${REPLACEMENT}/g" ${PATH_TO_ASSEMBLY_FILE} > ${PATH_TO_ASSEMBLY_FILE}.temp

cp -T ${PATH_TO_ASSEMBLY_FILE}.temp ${PATH_TO_ASSEMBLY_FILE}
rm ${PATH_TO_ASSEMBLY_FILE}.temp